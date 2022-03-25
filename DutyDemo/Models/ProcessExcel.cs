using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web;

namespace DutyDemo.Models
{
    public class ProcessExcel
    {
        public Dictionary<string, double> importDetails { get; set; }
        public Dictionary<string, double> exciseDetails { get; set; }
        public Dictionary<string, string> yearWiseImportDuties { get; set; }
        public Dictionary<string, string> yearWiseExciseDuties { get; set; }
        public ProcessExcel()
            {
                importDetails = new Dictionary<string, double>();
                exciseDetails = new Dictionary<string, double>();
                yearWiseImportDuties = new Dictionary<string, string>();
                yearWiseExciseDuties = new Dictionary<string, string>();
        }
     
        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(strFilePath))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }

                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    if (rows.Length > 1)
                    {
                        DataRow dr = dt.NewRow();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            dr[i] = rows[i].Trim();
                        }
                        dt.Rows.Add(dr);
                    }
                }
            }
            return dt;
        }

        public Dictionary<string,double> FindYearsHighRevenueImportDuties(DataTable dt) //column wise
        {
            var model = new ProcessExcel();
            var importColumns = dt.Columns.Cast<DataColumn>().Where(c => c.ColumnName.Contains("Import Duty")).ToList();
                foreach (var item in importColumns)
            {
                var a = dt.AsEnumerable()
                        .Select(x => Convert.ToDouble(x.Field<string>(item.ColumnName)))
                        .DefaultIfEmpty(0)
                        .Max(x => x);

                var result = dt.AsEnumerable()
                                .Where(at => Convert.ToDouble(at.Field<string>(item.ColumnName)) == a)
                                .Select(x => x.Field<string>("Year (Upto 31st March) (Col.1)")).FirstOrDefault();

                if (!importDetails.ContainsKey(result))
                    importDetails.Add(result, a);
            }
            return importDetails;
        }

        public Dictionary<string,double> FindYearsHighRevenueExciseDuties(DataTable dt) //column wise
        {
            var exciseColumns = dt.Columns.Cast<DataColumn>().Where(c => c.ColumnName.Contains("Excise Duty")).ToList();
                       foreach (var item in exciseColumns)
            {
                var a = dt.AsEnumerable()
                        .Select(x => Convert.ToDouble(x.Field<string>(item.ColumnName)))
                        .DefaultIfEmpty(0)
                        .Max(x => x);

                var result = dt.AsEnumerable()
                                .Where(at => Convert.ToDouble(at.Field<string>(item.ColumnName)) == a)
                                .Select(x => x.Field<string>("Year (Upto 31st March) (Col.1)")).FirstOrDefault();

                if (!exciseDetails.ContainsKey(result))
                    exciseDetails.Add(result, a);
            }
            return exciseDetails;
        }

        public Dictionary<string,string> FindYearWiseMaxImportDuties(DataTable dt) //row wise
        {
              string[] selectedColumns = new[] { "Year (Upto 31st March) (Col.1)",
                "Central - Motor Vehicle & Accessories - Import Duty (Col.2)",
                "Central - Tyres and Tubes - Import Duty (Col.4)",
                "Central - High Speed Diesel Oil - Import Duty (Col.6)",
                "Central - Motor Spirit - Import Duty (Col.8)"};

            DataTable dts = new DataView(dt).ToTable(false, selectedColumns);
            DataTable ddd = UnpivotDataTable(dts); 
            foreach (var r in ddd.Columns.Cast<DataColumn>().Skip(1))
            {
                //Get max value
                var max = ddd.AsEnumerable().Skip(1)
                        .Select(x => Convert.ToDouble(x.Field<string>(r.ColumnName)))
                        .DefaultIfEmpty(0)
                        .Max(x => x);

                //Get category name
                var result = ddd.AsEnumerable().Skip(1)
                                .Where(at => Convert.ToDouble(at.Field<string>(r.ColumnName)) == max)
                                .Select(x => x.Field<string>("Headers")).FirstOrDefault();
                //get year
                var year = ddd.Rows[0].Field<string>(r.ColumnName);

                if (!yearWiseImportDuties.ContainsKey(year))
                    yearWiseImportDuties.Add(year, result);
            }
            return yearWiseImportDuties;
        }

        public Dictionary<string,string> FindYearWiseMaxExciseDuties(DataTable dt) //row wise
        {
            string[] selectedColumns = new[] { "Year (Upto 31st March) (Col.1)",
                "Central - Motor Vehicle & Accessories - Excise Duty (Col.3)",
                "Central - Tyres and Tubes - Excise Duty (Col.5)",
                "Central - High Speed Diesel Oil - Excise Duty (Col.7)",
                "Central - Motor Spirit - Excise Duty (Col.9)"};

            DataTable dts = new DataView(dt).ToTable(false, selectedColumns);
            DataTable ddd = UnpivotDataTable(dts); 
            foreach (var r in ddd.Columns.Cast<DataColumn>().Skip(1))
            {
                //Get max value
                var max = ddd.AsEnumerable().Skip(1)
                        .Select(x => Convert.ToDouble(x.Field<string>(r.ColumnName)))
                        .DefaultIfEmpty(0)
                        .Max(x => x);

                //Get category name
                var result = ddd.AsEnumerable().Skip(1)
                                .Where(at => Convert.ToDouble(at.Field<string>(r.ColumnName)) == max)
                                .Select(x => x.Field<string>("Headers")).FirstOrDefault();
                //get year
                var year = ddd.Rows[0].Field<string>(r.ColumnName);

                if (!yearWiseExciseDuties.ContainsKey(year))
                    yearWiseExciseDuties.Add(year, result);
            }
            return yearWiseExciseDuties;
        }

        public static DataTable UnpivotDataTable(DataTable dt)
        {
            string[] columnNames = dt.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
            var dtColumnWise = new DataTable("unpivot");
            dtColumnWise.Columns.Add("Headers", typeof(string));
            for (int rowIndex = 0; rowIndex < dt.Rows.Count; rowIndex++)
            {
                dtColumnWise.Columns.Add("Row" + rowIndex.ToString(), typeof(string));
            }

            for (int i = 0; i < columnNames.Length; i++)
            {
                dtColumnWise.Rows.Add(columnNames[i], dt.Rows[0].ItemArray[i], dt.Rows[1].ItemArray[i],
                    dt.Rows[2].ItemArray[i], dt.Rows[3].ItemArray[i],
                    dt.Rows[4].ItemArray[i], dt.Rows[5].ItemArray[i],
                    dt.Rows[6].ItemArray[i], dt.Rows[7].ItemArray[i],
                    dt.Rows[8].ItemArray[i], dt.Rows[9].ItemArray[i],
                    dt.Rows[10].ItemArray[i], dt.Rows[11].ItemArray[i],
                    dt.Rows[12].ItemArray[i], dt.Rows[13].ItemArray[i],
                    dt.Rows[14].ItemArray[i], dt.Rows[15].ItemArray[i],
                    dt.Rows[16].ItemArray[i]);
            }
            return dtColumnWise;
        }
    }
}
