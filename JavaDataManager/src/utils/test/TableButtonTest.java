package utils.test;

import java.awt.EventQueue;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;

import javax.swing.JFrame;
import javax.swing.JPanel;
import javax.swing.JTable;
import javax.swing.border.EmptyBorder;
import javax.swing.table.DefaultTableModel;
import javax.swing.table.TableColumn;

import utils.table.TableButton;

@SuppressWarnings("serial")
public class TableButtonTest extends JFrame {

	private JPanel contentPane;
	private JTable table;

	/**
	 * Launch the application.
	 */
	public static void main(String[] args) {
		EventQueue.invokeLater(new Runnable() {
			public void run() {
				try {
					TableButtonTest frame = new TableButtonTest();
					frame.setVisible(true);
				} catch (Exception e) {
					e.printStackTrace();
				}
			}
		});
	}

	/**
	 * Create the frame.
	 */
	public TableButtonTest() {
		setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		setBounds(200, 200, 450, 300);
		contentPane = new JPanel();
		contentPane.setBorder(new EmptyBorder(5, 5, 5, 5));
		setContentPane(contentPane);
		contentPane.setLayout(null);
		
		table = new JTable();
		table.setModel(new DefaultTableModel(
			new Object[][] {
				{"Test", "4"},
				{"Juca", "6"},
				{"Faca", "7"},
				{"olha", "6"},
			},
			new String[] {
				"Nome", "Codigo"
			}
		));
		TableButton buttonEditor = new TableButton(30);
		buttonEditor.setText("Teste");
		buttonEditor.addActionListener(new ActionListener() {
			
			@Override
			public void actionPerformed(ActionEvent arg0) {
				
			}
		});

		TableButton edit=new TableButton(30);
		edit.addActionListener(new ActionListener() {			
			@Override
			public void actionPerformed(ActionEvent e) {
				TableButton btn=(TableButton) e.getSource();
				System.out.println("Click na linha "+btn.getRow()+" e na coluna "+btn.getColumn());				
			}
		});
		
		TableColumn col = new TableColumn(1, 30);
		col.setMaxWidth(30);
		col.setCellRenderer(edit);
		col.setCellEditor(edit);

		table.addColumn(col);
		
		table.setBounds(0, 0, 400, 250);
		contentPane.add(table);
		
		
	}
}
