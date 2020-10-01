
namespace vykuttolib.Services.PhotoProcessing
{
    public class Slice
    {
        public enum SliceDirection
        {
            Horizontal = 0,
            Vertical = 1
        }

        public SliceDirection Direction { get; set; }
        public int Coordinate { get; set; }
    }
}
