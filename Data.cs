using System.Drawing;

namespace RanksGenerator
{
    internal struct Data
    {
        public Image Img;
        public string Progress;
        public Point Pack;
        public bool BtnEnable;
        public string BtnText;

        public Data(Image img, string prog, Point pack, bool btn, string btext)
        {
            Img = img;
            Progress = prog;
            Pack = pack;
            BtnEnable = btn;
            BtnText = btext;
        }
    }
}