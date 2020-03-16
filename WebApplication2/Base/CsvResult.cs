using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Reflection;

namespace SampleWebApp.Base
{
    public class CsvResult<T> : ActionResult
    {
        string field_separator = ",";
        string line_separator = "\r\n";

        private IEnumerable<T> data;
        private string filename;
        private string[] fields;
        private string[] labels;

        public CsvResult(IEnumerable<T> data, string filename = null, string[] fields = null, string[] labels = null)
        {
            if(data == null)
            {
                throw new ArgumentNullException("data should not be null.");
            }

            this.data = data;
            this.filename = filename ?? string.Format("{0}_{1:yyyyMMdd_HHmmss_fff}.csv", typeof(T).Name, DateTime.Now);
            this.fields = fields ?? typeof(T).GetProperties().Select(a => a.Name).ToArray();
            this.labels = labels ?? this.fields;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var resp = context.HttpContext.Response;
            resp.ContentEncoding = System.Text.Encoding.UTF8;
            resp.ContentType = "text/csv";
            resp.Headers.Add("Content-Disposition", string.Format("attachment; filename=\"{0}\"", filename));

            var props = typeof(T).GetProperties();
            var csvProps = this.fields.Select(a => props.FirstOrDefault(b => b.Name == a));

            // BOM
            resp.Write('\uFEFF');

            // Field Header
            resp.Write(string.Join(field_separator, this.labels));
            resp.Write(line_separator);

            // Data records
            foreach(var record in data)
            {
                resp.Write(string.Join(field_separator, csvProps.Select(p => this.CreateField(p?.GetValue(record))).ToArray()));
                resp.Write(line_separator);
            }
        }

        public string CreateField(object rawdata)
        {
            if(rawdata == null)
            {
                return string.Empty;
            }

            var value = rawdata.ToString();

            value = value.Replace("\"", "\"\"");

            value = value.Replace("\r\n", "\n");

            return string.Format("\"{0}\"", value);
        }
    }
}