using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace TableBuilderLibrary
{
    public static class TableBuilder
    {
        public static DataTable BuildTableSchema(string tableName, string[] headers, bool needsGuid)
        {
            //Create DataTable, Create DataColumns, and Add DataColumns to DataTable
            List<string> newHeaders = headers.ToList();

            if (needsGuid == true) { newHeaders.Insert(0, "Guid"); }
            DataTable table = new DataTable(tableName);

            DataColumn[] columns = new DataColumn[newHeaders.Count];
            DataColumn[] key = new DataColumn[1];
            for (int x = 0; x < columns.Length; x++)
            {
                bool readOnly = false;
                bool isUnique = false;
                Type type = typeof(string);
                string headerName = "";
                if (x == 0)
                {
                    readOnly = true;
                    isUnique = true;
                    if (needsGuid == true)
                    {
                        type = typeof(Guid);
                        headerName = "Guid";
                    }
                    else
                    {
                        headerName = newHeaders[x];
                    }
                }
                else
                {
                    headerName = newHeaders[x];
                }
                columns[x] = CreateColumn(type, headerName, readOnly, isUnique);
            }
            key[0] = columns[0];
            table = AddColumnsToTable(table, columns);
            if (needsGuid)
            {
                table.PrimaryKey = key;
            }
            
            return table;
        }
        public static DataTable BuildTableSchema(string tableName, string[] headers, Type[] columnTypes, bool needsGuid)
        {
            //Create DataTable, Create DataColumns, and Add DataColumns to DataTable
            List<string> newHeaders = headers.ToList();
            List<Type> types = columnTypes.ToList();
            if (needsGuid == true) { newHeaders.Insert(0, "Guid"); types.Insert(0, typeof(Guid)); }
            DataTable table = new DataTable(tableName);

            DataColumn[] columns = new DataColumn[newHeaders.Count];
            DataColumn[] key = new DataColumn[1];
            for (int x = 0; x < columns.Length; x++)
            {
                bool readOnly = false;
                bool isUnique = false;
                if (x == 0 && needsGuid == true)
                {
                    readOnly = true;
                    isUnique = true;
                }
                columns[x] = CreateColumn(types[x], newHeaders[x], readOnly, isUnique);
            }
            key[0] = columns[0];
            table = AddColumnsToTable(table, columns);
            if (needsGuid)
            {
                table.PrimaryKey = key;
            }
            return table;
            ////Create DataTable, Create DataColumns, and Add DataColumns to DataTable
            //DataTable table = new DataTable(tableName);
            //DataColumn[] columns = new DataColumn[headers.Length];
            //for (int x = 0; x < headers.Length; x++)
            //{
            //    bool readOnly = false;
            //    bool isUnique = false;
            //    if (x == 0) { readOnly = true; isUnique = true; }

            //    DataColumn column = CreateColumn(columnTypes[x].ToString(), headers[x], readOnly, isUnique);
            //    columns[x] = column;
            //}
            //table = AddColumnsToTable(table, columns);
            //return table;
        }
        //public void buildTableSchemaFromDatabase()
        //{
        //    //Create DataSet, Update Catalog DataTable, Create DataColumns, and Add DataColumns to DataTable

        //    table = Dataset.Tables.Add(Configuration.TableName); //Get name of table

        //    List<DataColumn> columns = new List<DataColumn>(new DataColumn[] { columnListGoesHere });
        //    table = addColumnsToTable(columns);
        //    //columnCount = columns.Count();
        //}
        public static DataTable PopulateTableFromCsv(this DataTable table, string folderPath, string fileName, char delimiter, bool hasHeaders)
        {
            List<string> csv = System.IO.File.ReadAllLines(folderPath + "\\" + fileName + ".csv").ToList();
            int startIndex = 0;
            if (hasHeaders == true)
            {
                startIndex = 1; //allows for skipping of headers
            }

            for (int x = startIndex; x < csv.Count; x++)
            {
                Object[] rowContent = csv[x].Replace("\"", "").Split(delimiter); //Separates out each element in between quotes
                DataRow row = CreateDataRow(table, AssignTypesToData(table, rowContent)); //creates DataRow with data types that match the table schema
                table.Rows.Add(row);
            }
            return table;
        }
        public static DataTable PopulateTableFromCsv(this DataTable table, string fullFilePath, char delimiter, bool hasHeaders)
        {
            List<string> csv = System.IO.File.ReadAllLines(fullFilePath + ".csv").ToList();
            int startIndex = 0;
            if (hasHeaders == true)
            {
                startIndex = 1; //allows for skipping of headers
            }

            for (int x = startIndex; x < csv.Count; x++)
            {
                csv[x] = csv[x].Insert(0, "");
                Object[] rowContent = csv[x].Replace("\"", "").Split(delimiter); //Separates out each element in between quotes
                DataRow row = CreateDataRow(table, AssignTypesToData(table, rowContent)); //creates DataRow with data types that match the table schema
                table.Rows.Add(row);
                if (table.Rows.Count % 1000 == 0)
                {
                    Console.WriteLine("Row #" + table.Rows.Count + " added successfully. ");
                }
            }
            return table;
        }
        public static DataTable PopulateTableFromCsv(this DataTable table, List<string> csv, char delimiter, bool hasHeaders)
        {
            int startIndex = 0;
            if (hasHeaders == true)
            {
                startIndex = 1; //allows for skipping of headers
            }

            for (int x = startIndex; x < csv.Count; x++)
            {
                csv[x] = csv[x].Insert(0, "");
                Object[] rowContent = csv[x].Replace("\"", "").Split(delimiter); //Separates out each element in between quotes
                DataRow row = CreateDataRow(table, AssignTypesToData(table, rowContent)); //creates DataRow with data types that match the table schema
                table.Rows.Add(row);
                if (table.Rows.Count % 1000 == 0)
                {
                    Console.WriteLine("Row #" + table.Rows.Count + " added successfully. ");
                }
            }
            return table;
        }
        public static DataTable AddColumnsToTable(this DataTable table, IEnumerable<DataColumn> columns)
        {
            foreach (DataColumn column in columns)
            {
                table.Columns.Add(column);
            }
            return table;
        }
        public static DataColumn CreateColumn(Type columnType, string columnName, Boolean readOnly, Boolean isUnique)
        {
            // Create new DataColumn, set DataType, 
            // ColumnName and add to DataTable.    
            DataColumn column = new DataColumn
            {
                DataType = columnType,//Type.GetType(columnType),
                ColumnName = columnName,
                ReadOnly = readOnly,
                Unique = isUnique
            };
            return column;
        }
        public static DataTable SetPrimaryKeyColumn(this DataTable table, string primaryColumnName)
        {
            // Make the ID column the primary key column.
            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
            PrimaryKeyColumns[0] = table.Columns[primaryColumnName];
            table.PrimaryKey = PrimaryKeyColumns;
            return table;
        }
        public static DataRow CreateDataRow(DataTable table, Object[] cellData)
        {
            List<string> columnNames = new List<string>();
            foreach (DataColumn column in table.Columns) //Gets list of all column names in the table
            {
                columnNames.Add(column.ColumnName);
            }
            string name = table.Columns[0].DataType.Name.ToString();
            int offset = 0;
            table.TableNewRow += new
                DataTableNewRowEventHandler(Table_NewRow);
            DataRow newRow = table.NewRow();
            for (int x = 0; x < newRow.Table.Columns.Count; x++)
            {
                if (newRow.Table.Columns[x].DataType == Type.GetType("System.Guid"))
                {
                    offset++;
                }
                else
                {
                    newRow[x] = cellData[x - offset];
                }
            }
            return newRow;
        }
        private static void Table_NewRow(object sender, DataTableNewRowEventArgs e)
        {
            e.Row[0] = Guid.NewGuid();
        }
        public static Object[] AssignTypesToData(DataTable table, Object[] data)
        {
            Object[] convertedDatas = new Object[data.Length];
            List<DataColumn> columns = new List<DataColumn>();
            int offset = 0;
            if (table.Columns[0].DataType.Name.ToString().Equals("Guid", StringComparison.OrdinalIgnoreCase))
            {
                offset = 1;
            }
            for (int x = 0; x < data.Length; x++)
            {
                convertedDatas[x] = Convert.ChangeType(data[x], table.Columns[x + offset].DataType);
            }
            return convertedDatas;
        }
        public static List<object> GetAllDataFromColumn(DataTable table, string columnName)
        {
            int columnIndex = table.Columns[columnName].Ordinal;
            List<object> listOfDataFromColumn = new List<object>(table.Rows.Count);
            foreach (DataRow row in table.Rows)
            {
                listOfDataFromColumn.Add((object)row[columnIndex]);
            }
            return listOfDataFromColumn;
        }
        public static List<object> GetAllDataFromColumn(DataTable table, int columnIndex)
        {
            List<object> listOfDataFromColumn = new List<object>(table.Rows.Count);
            foreach (DataRow row in table.Rows)
            {
                listOfDataFromColumn.Add((object)row[columnIndex]);
            }
            return listOfDataFromColumn;
        }
        public static Type GetColumnType(DataTable table, string columnName)
        {
            Type columnType = table.Columns[columnName].DataType;
            return columnType;
        }
        public static Type GetColumnType(DataTable table, int ordinalPosition)
        {
            Type columnType = table.Columns[ordinalPosition].DataType;
            return columnType;
        }
        public static DataTable AddRowToTable(this DataTable table, DataRow row)
        {
            table.Rows.Add(row);
            return table;
        }
        public static string[] GetHeaders(List<string> lines, char delimiter)
        {
            string[] headers = lines[0].Split(delimiter);
            return headers;
        }
        public static string[] GetHeaders(string filePath, char delimiter)
        {
            string[] headers = File.ReadLines(filePath).First().Split(delimiter).ToList().Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            return headers;
        }
        public static DataTable CSVtoDataTable(string tableName, Type[] tableColumnTypes, List<string> csv, char delimiter, bool defaultSchema, bool needsGuid)
        {
            //Check if file exists
            //if (csv.Count)) //If exists
            //{
            //Console.WriteLine("CSV file exists. Importing...");
            //Import file
            //List<string> csv = ImportCsvToStringList(tableFilePath);

            DataTable table = new DataTable();
            if (defaultSchema == true)
            {
                table = BuildTableSchema(tableName, GetHeaders(csv, delimiter), needsGuid);
            }
            else
            {
                table = BuildTableSchema(tableName, GetHeaders(csv, delimiter), tableColumnTypes, needsGuid);
            }

            //Populate table from file
            //if (tableFilePath.EndsWith(".csv"))
            //{
            //    tableFilePath = tableFilePath.Remove(tableFilePath.Length - 4);
            //}

            table.PopulateTableFromCsv(csv, delimiter, true);
            return table;
            //}
            //else //If not exists
            //{
            //    Console.WriteLine("CSV file does not exists. Generating...");
            //    //Build table from scratch
            //    DataTable table = TableBuilderLibrary.TableBuilder.BuildTableSchema(Configuration.TableName, Configuration.TableHeaders, Configuration.TableColumnTypes);
            //    ExportDataTableToCSV(table, Configuration.TableFolderPath, Configuration.TableName);
            //    Console.WriteLine("CSV file saved.");
            //    return table;
            //}
        }
        //public void exportTableSchemaToFile(DataTable table)
        //{
        //    List<string> lines = new List<string>();
        //    foreach (DataColumn column in table.Columns)
        //    {
        //        lines.Add("DataType=" + column.DataType.ToString());
        //        lines.Add("ColumnName=" + column.ColumnName.ToString());
        //        lines.Add("ReadOnly=" + column.ReadOnly.ToString());
        //        lines.Add("Unique=" + column.Unique.ToString());
        //    }
        //}
    }
}
