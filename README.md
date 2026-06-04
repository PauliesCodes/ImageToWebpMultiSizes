# Web Image Batch Optimizer

A lightweight desktop application designed to batch resize images and convert them into the modern, web-optimized **WebP** format.

---

## Quick Start / Installation

To run the application on your computer:

1. **Download** the archive named `ImageToWebpMultiSizes Alpha.zip` from this repository.
2. **Extract** the contents of the ZIP file to a folder of your choice.
3. Run the **setup** file (e.g., `Setup.exe` or the setup installer) inside the extracted folder to install and launch the application.

---

## Why This Project Exists

Most online image converters come with frustrating limitations:
* **File limits:** They restrict the number of files you can process at once.
* **Size constraints:** Large images are often blocked unless you pay.
* **Paywalls:** Access to fast or unlimited batch processing is frequently hidden behind subscriptions.
* **Slow speeds:** Uploading high-resolution PNG or JPG files to a server and downloading them back takes significant time.

This application was developed to offer a **local, offline, and completely free alternative**. By running the conversion directly on your computer, it eliminates file size limits, subscription costs, and upload bottlenecks.

---

## Key Features

* **Multi-Dimension Task Management:**
  * Define multiple target heights (e.g., 800px, 400px, 200px) in a single session.
  * Individually map specific images to different target dimensions.
  * Proportional resizing based on height prevents image distortion.

* **Visual Color-Coding:**
  * Each target dimension is assigned a unique pastel color.
  * Selected image cards are highlighted with the color of their active dimension.
  * Small color indicators (dots) on the bottom of each card display all assigned sizes for that image.

* **Efficient Workflows:**
  * Displays thumbnail previews, file names, and individual file sizes for all images in the source folder.
  * **Select All** and **Deselect All** buttons allow quick bulk assignment for the currently active dimension.
  * Keeps the original file name, changing only the extension to `.webp` for straightforward web deployment.

* **Data Savings & Statistics:**
  * Calculates and displays the total size of all loaded source files.
  * Computes the final size of the output WebP files and displays the compression ratio (e.g., "5.4x smaller") upon completion.

* **Asynchronous Performance:**
  * Built using asynchronous C# patterns (`async/await`) to ensure the user interface remains responsive during background thumbnail loading and heavy image compression.

---

## Technical Specifications

* **Language & Framework:** C# with Windows Forms (.NET)
* **Image Processing Engine:** **Magick.NET** (ImageMagick wrapper for C#) for high-quality WebP encoding and proportional resizing.
* **I/O Handling:** Non-locking file stream access to prevent file locks in the operating system during thumbnail generation.
