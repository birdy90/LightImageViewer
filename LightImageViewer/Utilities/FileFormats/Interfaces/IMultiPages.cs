namespace LightImageViewer.FileFormats
{
    /// <summary>
    /// Interface for images with multiple pages
    /// </summary>
    public interface IMultiPages
    {
        int CurrentPage { get; set; }
        int PagesCount { get; }

        void NextPage();
        void PreviousPage();
    }
}
