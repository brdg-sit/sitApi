using Microsoft.ML.Data;

namespace EC_API.Models
{
    public class LoadCool
    {
        [LoadColumn(0)]
        public float aspect_ratio { get; set; }

        [LoadColumn(1)]
        public float temp_cool { get; set; }

        [LoadColumn(2)]
        public float pwr_eqmt { get; set; }

        [LoadColumn(3)]
        public float temp_heat { get; set; }

        [LoadColumn(4)]
        public float level_light { get; set; }

        [LoadColumn(5)]
        public float north_axis { get; set; }

        [LoadColumn(6)]
        public float occupancy { get; set; }

        [LoadColumn(7)]
        public float shgc { get; set; }

        [LoadColumn(8)]
        public float u_floor { get; set; }

        [LoadColumn(9)]
        public float u_roof { get; set; }

        [LoadColumn(10)]
        public float u_wall { get; set; }

        [LoadColumn(11)]
        public float u_window { get; set; }

        [LoadColumn(12)]
        public float hur_wday { get; set; }

        [LoadColumn(13)]
        public float hur_wend { get; set; }

        [LoadColumn(14)]
        public float wwr { get; set; }

        [LoadColumn(15)]
        public float effcy_cool { get; set; }

        [LoadColumn(16)]
        public float effcy_heat { get; set; }

        [LoadColumn(17)]
        [ColumnName("Label")]
        public float load_cool { get; set; }
    }
}
