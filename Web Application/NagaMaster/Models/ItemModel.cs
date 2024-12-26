using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NagaMaster.Models
{
    public class ItemModel : LayoutModel
    {
        public int Id { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialDesc { get; set; }
        public string BUOMName { get; set; }
        public string ItemShortName1 { get; set; }
        public string ItemShortName2 { get; set; }
        public int Denominator { get; set; }
        public string AUOMName { get; set; }
        public int Numerator { get; set; }
        public int BoxQuantityInCrate { get; set; }
        public int ShelflifeMin { get; set; }
        public int ShelflifeMax { get; set; }
        public string PeriodIndicatorSLED { get; set; }
        public string ProdGroupCode { get; set; }
        public string ProdGroupDesc { get; set; }
        public string SizeInInchCode { get; set; }
        public string SizeInInchDesc { get; set; }
        public decimal GrossWgt { get; set; }
        public decimal NetWgt { get; set; }
        public decimal MinTolerance { get; set; }
        public decimal MaxTolerance { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public List<importModel> ExcelImport { get; set; }

        public static implicit operator ItemModel(SettingsModel v)
        {
            throw new NotImplementedException();
        }

        public static implicit operator ItemModel(UnitModel v)
        {
            throw new NotImplementedException();
        }
        public class importModel
        {
            //public int Id { get; set; }
            public string MaterialCode { get; set; }
            public string MaterialDesc { get; set; }
            public string BUOMName { get; set; }
            public string ItemShortName1 { get; set; }
            public string ItemShortName2 { get; set; }
            public int Denominator { get; set; }
            public string AUOMName { get; set; }
            public int Numerator { get; set; }
            public int BoxQuantityInCrate { get; set; }
            public int ShelflifeMin { get; set; }
            public int ShelflifeMax { get; set; }
            public string PeriodIndicatorSLED { get; set; }
            public string ProdGroupCode { get; set; }
            public string ProdGroupDesc { get; set; }
            public string SizeInInchCode { get; set; }
            public string SizeInInchDesc { get; set; }
            public decimal GrossWgt { get; set; }
            public decimal NetWgt { get; set; }
            public decimal MinTolerance { get; set; }
            public decimal MaxTolerance { get; set; }
            public int Status { get; set; }
            public string CreatedBy { get; set; }
            public DateTime CreatedOn { get; set; }
            public string ModifiedBy { get; set; }
            public DateTime ModifiedOn { get; set; }
        }
    }
}