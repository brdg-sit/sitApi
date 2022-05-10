using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace UnrealViewerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DwgCollection : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private Transaction transaction = new Transaction();

        public DwgCollection(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        [HttpGet]
        [Route("filters")]
        public string GetFilters()
        {
            string query = $"SELECT A.CD_PRNT, A.CD_COMM, A.NM_COMM " +
                $"FROM TB_DWG_COMM A " +
                $"WHERE A.CD_PRNT = 'G' or A.CD_PRNT = 'H'";

            string query2 = $"SELECT 'R' AS CD_PRNT, CAST (A.ID AS nvarchar) AS CD_COMM, A.NM_ROOM as NM_COMM " +
                $"FROM TB_DWG_ROOM_TYPE A";

            string dataSource = _configuration.GetConnectionString("RealServerConnectionString");

            var dataTable = transaction.GetTableFromDB(query, dataSource);
            var dataTable2 = transaction.GetTableFromDB(query2, dataSource);

            
            Filters filters = new Filters();
            foreach (DataRow row in dataTable.Rows)
            {
                var cd = row["CD_PRNT"].ToString();
                //var filter = new Filter() { value = row["CD_COMM"].ToString(), name = row["NM_COMM"].ToString() };
                var filter = new Filter() { name = row["NM_COMM"].ToString() };
                if (cd == "G")
                {
                    filters.Element.Add(filter);
                }
                else
                {
                    filters.Facility.Add(filter);
                }
            }

            foreach (DataRow row in dataTable2.Rows)
            {
                //var filter = new Filter() { value = row["CD_COMM"].ToString(), name = row["NM_COMM"].ToString() };
                var filter = new Filter() { name = row["NM_COMM"].ToString() };
                filters.Room.Add(filter);
            }

            return JsonConvert.SerializeObject(filters);
        }

        //[HttpGet]
        //[Route("collections")]
        //public async Task<ActionResult<Collection>> GetAllCollections()
        //{
        //    string query = $"SELECT CD_PROJ, NM_PROJ, CD_SHTTYP, CD_STEP, NM_ROW, JSON_ROW, C.NM_COMM as SHEET, D.NM_COMM as STEP " +
        //        $"FROM TB_DWG_COLLECT_GROUP A " +
        //        $"LEFT JOIN TB_DWG_COLLECT_GROUP_ROW B on A.ID = B.ID_PROJ " +
        //        $"LEFT JOIN TB_DWG_COMM C on A.CD_SHTTYP = C.CD_COMM " +
        //        $"LEFT JOIN TB_DWG_COMM D on A.CD_STEP = D.CD_COMM";

        //    string dataSource = _configuration.GetConnectionString("RealServerConnectionString");

        //    var dataSet = await transaction.GetDataSetAsync(dataSource, query, 0, 5);

        //    Collection collection = new Collection();

        //    if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
        //    {
        //        foreach (DataRow row in dataSet.Tables[0].Rows)
        //        {
        //            var CD_PROJ = row["CD_PROJ"].ToString();
        //            var NM_PROJ = row["NM_PROJ"].ToString();
        //            var SHEET = row["SHEET"].ToString();
        //            var STEP = row["STEP"].ToString();
        //            var NM_ROW = row["NM_ROW"].ToString();
        //            var JSON_ROW = row["JSON_ROW"].ToString();

        //            var rowData = JsonConvert.DeserializeObject<RowCollection>(JSON_ROW);

        //            var project = collection.Data.FirstOrDefault(x => x.ProjCd == CD_PROJ);

        //            if (project == null)
        //            {
        //                project = new Projects()
        //                {
        //                    ProjCd = CD_PROJ,
        //                    ProjNm = NM_PROJ,
        //                    Sheet = SHEET,
        //                    Elements = new List<Data>()
        //                };

        //                collection.Data.Add(project);
        //            }

        //            foreach (var member in rowData.Ranges)
        //            {
        //                foreach (var range in member.Value)
        //                {
        //                    foreach (var cell in range.Cells)
        //                    {
        //                        if (cell.Value == "-")
        //                            continue;

        //                        var elem = new Data() { Step = STEP, RoomName = rowData.RoomName, Element = member.Key, Type = cell.Key, Material = cell.Value };

        //                        if (project.Elements.Exists(x => x.RoomName == elem.RoomName && x.Element == elem.Element && x.Type == elem.Type && x.Material == elem.Material) == false)
        //                        {
        //                            project.Elements.Add(elem);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return collection;
        //}

        private static bool IsNotNull([NotNullWhen(true)] object? obj) => obj != null;

        [HttpPost]
        [Route("colls")]
        public async Task<ActionResult<Collection>> GetCollections(object obj, int cur, int page)
        {
            if (obj == null)
            {
                return new Collection();
            }

            JObject? jobj = JObject.Parse(obj.ToString());
            if (jobj.Count == 0)
            {
                return new Collection();
            }

            JArray? proj = (JArray?)jobj["프로젝트"];
            JArray? names = (JArray?)jobj["명칭"];
            JArray? rooms = (JArray?)jobj["공간"];
            JArray? elements = (JArray?)jobj["유형"];
            JArray? facilities = (JArray?)jobj["시설"];

            //int n = proj.Count + names.Count + rooms.Count + elements.Count + facilities.Count;
            //if (n == 0)
            //{
            //    return NotFound();
            //}


            /// 'like' 검색
            var projText = IsNotNull(proj) ? proj[0].ToString().Split(' ').ToList() : new List<string>();
            var namesText = IsNotNull(names) ? names[0].ToString().Split(' ').ToList() : new List<string>();

            /// '=' 검색
            var roomsText = IsNotNull(rooms) ? rooms.Select(c => (string?)c).ToList() : new List<string?>();
            var elementsText = IsNotNull(elements) ? elements.Select(c => (string?)c).ToList() : new List<string?>();
            var facilitiesText = IsNotNull(facilities) ? facilities.Select(c => (string?)c).ToList() : new List<string?>();

            string query =
                $"SELECT " +
                $"CD_PROJ, " +
                $"NM_PROJ, " +
                $"CONCAT(CD_PROJ, ' ', NM_PROJ) as PROJ, " +
                $"CD_SHTTYP, " +
                $"CD_STEP, " +
                $"NM_ROW, " +
                $"JSON_ROW, " +
                $"C.NM_COMM as SHEET, " +
                $"D.NM_COMM as STEP, " +
                $"B.NM_ROOM " +
                $"FROM TB_DWG_COLLECT_GROUP A " +
                $"LEFT JOIN TB_DWG_COLLECT_GROUP_ROW B on A.ID = B.ID_PROJ " +
                $"LEFT JOIN TB_DWG_COMM C on A.CD_SHTTYP = C.CD_COMM " +
                $"LEFT JOIN TB_DWG_COMM D on A.CD_STEP = D.CD_COMM";

            int i = 1;
            if (roomsText.Count > 0)
            {
                string str = " WHERE (";
                foreach (var rm in roomsText)
                {
                    str += $"B.NM_ROOM = '{rm}'";
                    if (i < roomsText.Count)
                    {
                        str += " or ";
                    }
                    i++;
                }
                query += $"{str}) ";
            }
            else
            {
                string str = " WHERE (B.NM_ROOM <> '') ";
                query += str;
            }

            i = 1;
            if (projText.Count > 0)
            {
                string str = " AND (";
                foreach (var pj in projText)
                {
                    str += $"(CONCAT(CD_PROJ, ' ', NM_PROJ) like '%{pj}%')";
                    if (i < projText.Count)
                    {
                        str += " or ";
                    }
                    i++;
                }
                query += $"{str})";
            }

            i = 1;
            if (namesText.Count > 0)
            {
                string str = " AND (";
                foreach (var nm in namesText)
                {
                    str += $"(B.JSON_ROW like '%{nm}%')";
                    if (i < namesText.Count)
                    {
                        str += " or ";
                    }
                    i++;
                }
                query += $"{str})";
            }


            string dataSource = _configuration.GetConnectionString("RealServerConnectionString");

            var dataSet = await transaction.GetDataSetAsync(dataSource, query, cur, page);

            Collection collection = new Collection();

            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    var CD_PROJ = row["CD_PROJ"].ToString();
                    var NM_PROJ = row["NM_PROJ"].ToString();
                    var SHEET = row["SHEET"].ToString();
                    var STEP = row["STEP"].ToString();
                    var NM_ROW = row["NM_ROW"].ToString();
                    var NM_ROOM = row["NM_ROOM"].ToString();
                    var JSON_ROW = row["JSON_ROW"].ToString();
                    if (JSON_ROW == null || JSON_ROW == "")
                        continue;

                    var rowData = JsonConvert.DeserializeObject<RowCollection>(JSON_ROW);

                    var project = collection.Data.FirstOrDefault(x => x.ProjCd == CD_PROJ);

                    if (project == null)
                    {
                        project = new Projects()
                        {
                            ProjCd = CD_PROJ,
                            ProjNm = NM_PROJ,
                            Sheet = SHEET,
                            Elements = new List<Data>()
                        };

                        collection.Data.Add(project);
                    }


                    var ranges = rowData.Ranges.ToList();

                    if (elementsText.Count > 0)
                    {
                        ranges = rowData.Ranges.Where(x => elementsText.Any(y => y == x.Key)).ToList();
                    }

                    foreach (var member in ranges)
                    {
                        foreach (var range in member.Value)
                        {
                            string sBase = string.Empty;
                            string sFinish = string.Empty;

                            foreach (var cell in range.Cells)
                            {
                                if (cell.Value == "-")
                                    continue;

                                if (cell.Key != "바탕" && cell.Key != "마감")
                                    continue;


                                if (namesText.Count > 0)
                                {
                                    if (namesText.Any(cell.Value.Contains))
                                    {
                                        if (cell.Key.Replace(" ", "") == "바탕")
                                        {
                                            sBase = cell.Value;
                                        }
                                        else if (cell.Key.Replace(" ", "") == "마감")
                                        {
                                            sFinish = cell.Value;
                                        }
                                    }
                                }
                                else
                                {
                                    if (cell.Key.Replace(" ", "") == "바탕")
                                    {
                                        sBase = cell.Value;
                                    }
                                    else if (cell.Key.Replace(" ", "") == "마감")
                                    {
                                        sFinish = cell.Value;
                                    }
                                }
                            }

                            if (sBase == string.Empty && sFinish == string.Empty)
                            {
                                continue;
                            }

                            var elem = new Data() { Step = STEP, RoomName = NM_ROOM, Element = member.Key, Base = sBase, Finish = sFinish };

                            if (project.Elements.Exists(x => x.RoomName == NM_ROOM && x.Element == elem.Element && x.Base == elem.Base && x.Finish == elem.Finish) == false)
                            {
                                project.Elements.Add(elem);
                            }
                        }
                    }
                }
            }


            return collection;
        }

        [HttpGet]
        [Route("img")]
        public string GetImgUrl(string room, string elem)
        {
            string query = 
                $"SELECT " +
                    $"URL_IMG " +
                $"FROM " +
                    $"TEMP_TB_DWG_IMG A " +
                    $"LEFT JOIN TEMP_TB_DWG_IMG_FINISH_REL B on B.ID_IMG = A.ID " +
                $"WHERE B.ID_ROOM = '{room}' AND B.ELEM_NM = '{elem}'";

            string dataSource = _configuration.GetConnectionString("RealServerConnectionString");

            var dataTable = transaction.GetTableFromDB(query, dataSource);

            if (dataTable.Rows.Count == 0)
                return "";

            return dataTable.Rows[0]["URL_IMG"].ToString();
        }



        //[HttpGet]
        //[Route("collsPaging")]
        //public async Task<ActionResult<Collection>> GetCollectionsPaging(int currentIndex, int pageSize)
        //{
        //    string query = $"SELECT CD_PROJ, NM_PROJ, CD_SHTTYP, CD_STEP, NM_ROW, JSON_ROW, C.NM_COMM as SHEET, D.NM_COMM as STEP, B.NM_ROOM " +
        //        $"FROM TB_DWG_COLLECT_GROUP A " +
        //        $"LEFT JOIN TB_DWG_COLLECT_GROUP_ROW B on A.ID = B.ID_PROJ " +
        //        $"LEFT JOIN TB_DWG_COMM C on A.CD_SHTTYP = C.CD_COMM " +
        //        $"LEFT JOIN TB_DWG_COMM D on A.CD_STEP = D.CD_COMM " +
        //        $"WHERE B.NM_ROOM <> ''";

        //    string dataSource = _configuration.GetConnectionString("RealServerConnectionString");

        //    var dataSet = await transaction.GetDataSetAsync(dataSource, query, 0, 5);

        //    Collection collection = new Collection();

        //    if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
        //    {
        //        foreach (DataRow row in dataSet.Tables[0].Rows)
        //        {
        //            var CD_PROJ = row["CD_PROJ"].ToString();
        //            var NM_PROJ = row["NM_PROJ"].ToString();
        //            var SHEET = row["SHEET"].ToString();
        //            var STEP = row["STEP"].ToString();
        //            var NM_ROW = row["NM_ROW"].ToString();
        //            var NM_ROOM = row["NM_ROOM"].ToString();
        //            var JSON_ROW = row["JSON_ROW"].ToString();

        //            var rowData = JsonConvert.DeserializeObject<RowCollection>(JSON_ROW);

        //            var project = collection.Data.FirstOrDefault(x => x.ProjCd == CD_PROJ);

        //            if (project == null)
        //            {
        //                project = new Projects()
        //                {
        //                    ProjCd = CD_PROJ,
        //                    ProjNm = NM_PROJ,
        //                    Sheet = SHEET,
        //                    Elements = new List<Data>()
        //                };

        //                collection.Data.Add(project);
        //            }

        //            foreach (var member in rowData.Ranges)
        //            {
        //                foreach (var range in member.Value)
        //                {
        //                    foreach (var cell in range.Cells)
        //                    {
        //                        if (cell.Value == "-")
        //                            continue;

        //                        var elem = new Data() { Step = STEP, RoomName = NM_ROOM, Element = member.Key, Type = cell.Key, Material = cell.Value };

        //                        if (project.Elements.Exists(x => x.RoomName == NM_ROOM && x.Element == elem.Element && x.Type == elem.Type && x.Material == elem.Material) == false)
        //                        {
        //                            project.Elements.Add(elem);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return collection;
        //}
    }

    public class Filters
    {
        [JsonProperty("공간")]
        public List<Filter> Room { get; set; }

        [JsonProperty("유형")]
        public List<Filter> Element { get; set; }

        [JsonProperty("시설")]
        public List<Filter> Facility { get; set; }

        public Filters()
        {
            Room = new List<Filter>();
            Element = new List<Filter>();
            Facility = new List<Filter>();
        }
    }

    public class Filter
    {
        public string? name { get; set; }
        //public string? value { get; set; }
    }


    public class Collection
    {
        public List<Projects> Data { get; set; }

        //public TaskAwaiter GetAwaiter()
        //{
        //    TaskAwaiter ta; // struct 타입이므로.
        //    return ta;
        //}
        public Collection()
        {
            Data = new List<Projects>();
        }
    }

    public class Projects
    {
        public string? ProjCd { get; set; }
        public string? ProjNm { get; set; }
        public string? Sheet { get; set; }

        public List<Data> Elements { get; set; }

        public Projects()
        {
            Elements = new List<Data>();
        }
    }

    public class Data
    {
        public string? Step { get; set; }
        public string? RoomName { get; set; }
        public string? Element { get; set; } // 바닥
        public string? Base { get; set; } // 바탕
        public string? Finish { get; set; } // 마감
    }


    public class RowCollection
    {
        public int Row { get; set; }

        public string RoomName { get; set; } = "";
        public string RoomNumber { get; set; } = "";
        public string Level { get; set; } = "";

        public List<Tuple<string, string, string>> RowDatas { get; set; }

        public Dictionary<string, List<RangeData>> Ranges { get; set; }

        public RowCollection()
        {
            RowDatas = new List<Tuple<string, string, string>>();
        }
    }

    public class RangeData
    {
        public List<CellData> Cells { get; set; }

        public RangeData()
        {
            Cells = new List<CellData>();
        }

        public override string ToString()
        {
            string str = "";
            foreach (var cell in Cells)
            {
                str += $"{cell.Key}:{cell.Value} |";
            }

            return str;
        }

    }

    public class CellData
    {
        // 바탕
        public string Key { get; set; }

        // 침투성방수
        public string Value { get; set; }

        public CellData(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Key} {Value}";
        }
    }
}
