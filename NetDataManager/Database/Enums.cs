
namespace Database
{
    /// <remarks/>
    public enum Operator
    {
        EQUAL,
        MORE_EQUAL,
        LESS_EQUAL,
        DIFFERENT,
        MORE,
        MINOR,
        AND,
        OR,
        OPEN_PARENTHESIS,
        CLOSE_PARENTHESIS,
        CONTAINS
    }
    /// <remarks/>
    public enum DATABASE_TYPE
    {
        /// <remarks/>
        SQLSERVER,
        /// <remarks/>
        MYSQL,
        /// <remarks/>
        SQLSERVERCE,
        SQLITE
    }

    /// <remarks/>
    public enum Status
    {
        /// <remarks/>
        New,
        /// <remarks/>
        Update,
        /// <remarks/>
        Delete,
        /// <remarks/>
        Normal,
        /// <remarks/>
        Invalid
    } 
}
