namespace TorchEconomySE.Models
{
    public class TradeZoneDataObject
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public int Range { get; set; }
        
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }

        public VRageMath.Vector3 Position
        {
            get { return new VRageMath.Vector3(PositionX, PositionY, PositionZ); }
        }
    }
}
