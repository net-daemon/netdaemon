namespace Model3.ModelV3.EventTypes
{
    public record ZhaEventData
    {
        public string device_ieee { get; set; }
        public string unique_id { get; set; }
        public int endpoint_id { get; set; }
        public int cluster_id { get; set; }
        public string command { get; set; }
        public object[] args { get; set; }
    }
}