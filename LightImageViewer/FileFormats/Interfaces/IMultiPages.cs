namespace LightImageViewer.FileFormats
{
    public interface IMultiPages
    {
        int CurrentPage { get; set; }
        int PagesCount { get; }

        void NextPage();
        void PreviousPage();
    }
}
