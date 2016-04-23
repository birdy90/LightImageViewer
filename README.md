# LightImageViewer

Light Image Viewer is a lightweight viewer for images with minimalistic intarface. It iss open-source and don't install any other background services

## Supported formats

Light Image Viewer supports following file formats:
* bmp
* png
* jpg (jpeg)
* gif (with animation)
* tif (tiff, including multiple pages)
* psd
* svg
* ico

## Hotkeys

As the program is very minimalistic and has no any usefull buttons or switches, Light Image Viewer have some funtions that can be executed using a hotkeys. There are these hotkeys:
* controlling image
	* **Up arrow** - scale up
	* **Down arrow** - scale down
	* **Del** - delete opened image
	* **Num0** - set initial size and position of image
	* **Ctrl+Mouse wheel** - scroll image vertically (centers image and scale to its top)
	* **Ctrl+Right arrow** - next page for Tif with multiple pages
	* **Ctrl+Left arrow** - previous page for Tif with multiple pages
* controlling viewer window
	* **Right arrow** - next image
	* **Left arrow** - previous image
	* **Ctrl+O** - open settings window
	* **Esc** - close viewer
* other functions
	* **P** - open with Photoshop (if installed)
	* **Ctrl+С** - copy image to clipboard
	* **Ctrl+D** - open current directory

## Some specific usecases

### Scrolling vertical images

When you has a long vertical image and want to scroll it, you can use mouse wheel with pressed control button for this. It will zoom image and let you scroll it vertically while the control button is pressed. It can be useful for watching long website mockups

### Copying an image

Sometimes you can get an image via skype and want to resend it to someone else. In that case you can copy current image with ctrl+C and paste it to other persons chat. But it not skype, you can paste an image weherever else you want - Office Word document, MSPaint, Adobe Photoshop, Outlook - any editor that supports pasting images from clipboard

### Open in photoshop

Instead of copypasting an image to Adobe Photoshop, you can just open it in Photoshop by pressing P on your keyboard. It determines if Photoshop is installed and launch it with your image