Small C# program to unpack an Urho3D .pak file made with the PackageTool.

I had stored some scenes I created for a managed app in a .pak file and then accidentally deleted the Urho editor data directory in which I'd stored them (whoops). No problem, I thought, because I still have the .pak file, so I'll just extract it using the PackageTool...only to find that it doesn't have an extract option.

In the Urho C++ code, it would have been fairly easy to use the PackageFile and File classes to read a package and write the contents to disk. The C# port of these classes does not (at the moment, Jul 2017) expose all methods to do that, so I had to take a peek at the original code and recreate the method to extract files from the pacakge. Murphy's law dictated that I'd used LZ4 compression, which was probably the hardest part to implement, but no biggie using Milosz Krajewski's lz4net nuget package.

DISCLAIMER: only tested in n=1 situation. It works for what I wanted to do with it. Enjoy but use with caution.

