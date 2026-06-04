Web Image Optimizer

A desktop application designed to batch resize images and convert them into the
modern, web-optimized WebP format.

Why this project exists

Most online image converters come with frustrating limitations: they restrict
the number of files you can upload, enforce strict file size limits, queue your
tasks, or require paid subscriptions for batch processing. Additionally,
uploading large source files to the cloud and downloading them back is often
slow and inefficient.

This application was created to provide a local, fast, and completely free
alternative. By running the conversion directly on your machine, there are no
file limits, no subscription fees, and no waiting for uploads to finish.

Key Features

  - Offline Batch Processing: Convert and resize hundreds of images locally with
    no internet connection required.
  - Multi-Dimension Mapping: Create multiple target heights
    (e.g., 800px, 400px, 200px) and assign specific images to different sizes in
    a single run.
  - Visual Color-Coding: Each target dimension is assigned a unique pastel
    color. The image cards in the gallery are highlighted with these colors,
    making it clear which image is assigned to which size.
  - Quick Select/Deselect: Easily select or deselect all images for the
    currently active dimension with a single click.
  - Original File Names Kept: The output files retain their original names,
    changing only the file extension to .webp for seamless integration into your
    web projects.
  - Size Reduction Statistics: The application displays the total size of the
    loaded source files and, upon completion, calculates the final WebP size and
    the exact compression ratio.
  - Asynchronous & Responsive UI: Built using async/await patterns, ensuring the
    application remains responsive during thumbnail generation and heavy image
    processing tasks.

Technologies Used

  - Language & UI: C# with Windows Forms (WinForms)
  - Image Processing Engine: Magick.NET (a robust .NET wrapper for ImageMagick),
    which handles high-quality WebP encoding and proportional resizing.
