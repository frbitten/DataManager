using System;
using Joo.Database.Attributes;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using Joo.Database.Types;
using Joo.Database.Events;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq.Expressions;

namespace Joo.Database
{
    [Serializable]
    public abstract class BasicModel : INotifyPropertyChanged
    {
        #region Constructor
        public BasicModel()
        {
            isSerializing = false;
            isSaving = false;
            Status = Status.New;
            ID = 0;
            LoadLevel = (int)Joo.Database.Connections.DataBaseConnection.LEVELS_OF_SEARCH.ALL * -1;
            IsFull = true;

            DatabaseType type = this.GetDatabaseType();
            foreach (var item in type.DataBaseStringProperties)
            {
                item.FastSetValue(this, string.Empty);
            }
        }
        #endregion

        #region [ Fields ]
        private Status status;
        private Int32 id;
        [NonSerialized]
        private bool isSaving;
        [NonSerialized]
        protected bool isSerializing;
        [NonSerialized]
        protected bool isLoading;
        #endregion

        #region [ Properties ]
        [Browsable(false)]
        public int LoadLevel
        {
            get;
            set;
        }

        [Browsable(false)]
        public Status Status
        {
            get { return status; }
            set
            {
                StatusChangedEventArgs args = new StatusChangedEventArgs();
                args.OldStatus = status;
                args.NewStatus = value;
                status = value;
                if (StatusChanged != null && !isSerializing && !isSaving)
                {
                    StatusChanged(this, args);
                }

                if (this.PropertyChanged != null && !isSerializing && !isSaving)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Status"));
                }
            }
        }

        [Browsable(false)]
        [FieldAttribute("id", FieldType.PRIMARY_KEY | FieldType.IDENTITY)]
        public virtual Int32 ID
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged(()=>ID);
            }
        }

        [Browsable(false)]
        public bool IsFull
        {
            get;
            set;
        }

        #endregion

        #region [ Overrides methods ]
        public override bool Equals(object obj)
        {
            if (this.GetType() != obj.GetType())
            {
                return false;
            }
            if ((obj as BasicModel).ID != this.ID)
            {
                return false;
            }
            else
            {
                if (this.Status != (obj as BasicModel).Status)
                {
                    return false;
                }
                if (this.ID == 0)
                {
                    return base.Equals(obj);
                }
            }

            return true;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region [ Public methods ]
        public virtual void Delete()
        {
            if (Status != Database.Status.New)
            {
                Status = Status.Delete;
            }
            else
            {
                Status = Database.Status.Invalid;
            }

            DatabaseType type = this.GetDatabaseType();
            foreach (var item in type.DataBaseRelationship)
            {
                if (item.Property.PropertyType.IsArray)
                {
                    BasicModel[] models = item.FastGetValue(this) as BasicModel[];
                    foreach (BasicModel model in models)
                    {
                        model.Delete();
                    }
                }
            }
        }
        public virtual void ChangeStatus(Status newStatus)
        {
            if (isSerializing)
            {
                return;
            }
            if (isSaving)
            {
                return;
            }

            switch (newStatus)
            {
                case Status.Update:
                    if (this.Status == Status.Normal)
                    {
                        this.Status = newStatus;
                    }
                    break;
                case Status.Delete:
                    if (this.Status == Status.Normal || this.Status == Status.Update)
                    {
                        this.Status = newStatus;
                    }
                    break;
                default:
                    this.Status = newStatus;
                    break;
            }
        }
        public DatabaseType GetDatabaseType()
        {
            return TypesManager.TypeOf(this);
        }

        public virtual void OnStartLoad()
        {
            isLoading = true;
        }
        public virtual void OnLoaded()
        {
            isLoading = false;
        }
        public virtual void OnStartSerializing()
        {
            isSerializing = true;
        }
        public virtual void onSerialized()
        {
            isSerializing = false;
        }

        public virtual T Clone<T>() where T : BasicModel
        {
            var stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
            byte[] buffer = stream.GetBuffer();
            stream.Close();
            stream = new MemoryStream(buffer);
            BasicModel ret = formatter.Deserialize(stream) as BasicModel;
            stream.Close();

            ret.isLoading = this.isLoading;
            ret.isSaving = this.isSaving;
            ret.isSerializing = this.isSerializing;

            return (T)ret;
        }

        [Obsolete("Método Obsoleto, utilizar método com genérics.")]
        public virtual BasicModel Clone()
        {
            var stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
            byte[] buffer = stream.GetBuffer();
            stream.Close();
            stream = new MemoryStream(buffer);
            BasicModel ret = formatter.Deserialize(stream) as BasicModel;
            stream.Close();

            ret.isLoading = this.isLoading;
            ret.isSaving = this.isSaving;
            ret.isSerializing = this.isSerializing;

            return ret;
        }
        #endregion

        #region [ INotifyPropertyChanged ]
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        [Obsolete("Utilizar o método que contém genéric.")]
        protected void OnPropertyChanged(string propertyName, bool changeStatus = true)
        {
            if (changeStatus)
                ChangeStatus(Status.Update);

            if (this.PropertyChanged != null && !isSerializing && !isSaving)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

        }


        protected void OnPropertyChanged<T>(Expression<Func<T>> expression, bool changeStatus = true)
        {
            if (changeStatus)
                ChangeStatus(Status.Update);

            if (this.PropertyChanged != null && !isSerializing && !isSaving)
                PropertyChanged(this, new PropertyChangedEventArgs(GetPropertyName(expression)));

        }

        protected string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            MemberExpression memberExpression = (MemberExpression)expression.Body;
            return memberExpression.Member.Name;
        }
        #endregion

        #region [ Deserialization Callback ]
        [OnDeserializing()]
        internal void OnDeserializing(StreamingContext context)
        {
            isSerializing = true;
        }
        [OnDeserialized()]
        internal void OnDeserialized(StreamingContext context)
        {
            isSerializing = false;
        }
        #endregion

        #region [ Events ]
        [field: NonSerialized]
        public event StatusChangedEventHandler StatusChanged;
        #endregion

        #region [ Delegates ]
        public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);
        #endregion

        #region [ Saving Callback ]
        internal void OnStartSaving()
        {
            isSaving = true;
        }
        internal void OnFinishSaving()
        {
            isSaving = false;
        }
        #endregion
    }
}