So you've decide to try to use this software.
It's not too complicated.

First, some terminology.

There are two types of regions to the configuration file, tags such as <set><\set>, <categories><\categories> and <preprocesses><\preprocesses>, and specifiers such as :name =, :root =, and :bitmaps =.

In the case of tags, the beginning <> and ending <\> tags must be located on separate lines, with the ending tag following the beginning tag.

The lines contained between two related tags are processed in a way appropriate for the tag name.

The <set><\set> tags specify an entire set of images that will be processed together. These are useful when you are processing entirely different sets of images but want a single config file to specify their batch processing at a single point.

All other tags and specifiers are expected to be contained within these tags.

Since this software suite was designed to be used with supervised learning methods, the <categories><\categories> tags are useful for differentiating the orthogonal groups of objects in the source data set. In the included example file the two categories are "Healthy" and "Diseased" since this project was originally developed to aid in the computer assisted detection of lung disease.

The <preprocess><\preprocess> tags define all of the various ways in which the original image set may have been processed prior to the running of this application. Since there are numerous techniques for image processing, it is conceivable that you may wish to create comparisons between them.

The :name specifier is used when reading and writing various files to differentiate between the input sets.

The :root specifier tells the application where to write out all of the produced data files. They will be written to a new directory at the specified location.

The :bitmaps specifier tells the program the location of the root source directory for the images.


Consider the first <set><\set> in the provided AutoProcess.cfg file.

For this set images will be read from
C:\Personal Files\Projects\AutoIPP\AutoProcess\Bitmap\Independent\Healthy\Unprocessed\
C:\Personal Files\Projects\AutoIPP\AutoProcess\Bitmap\Independent\Healthy\Process1\
C:\Personal Files\Projects\AutoIPP\AutoProcess\Bitmap\Independent\Healthy\Process2\
C:\Personal Files\Projects\AutoIPP\AutoProcess\Bitmap\Independent\Healthy\Process3\
C:\Personal Files\Projects\AutoIPP\AutoProcess\Bitmap\Independent\Diseased\Unprocessed\
C:\Personal Files\Projects\AutoIPP\AutoProcess\Bitmap\Independent\Diseased\Process1\
C:\Personal Files\Projects\AutoIPP\AutoProcess\Bitmap\Independent\Diseased\Process2\
C:\Personal Files\Projects\AutoIPP\AutoProcess\Bitmap\Independent\Diseased\Process3\

The output data files will be written to
C:\Personal Files\Projects\AutoIPP\AutoProcess\Data\

With subdirectories organizing the raw data files in a manner consistent with the input directory structure.




