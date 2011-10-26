rmdir /Q /S ..\Release
mkdir ..\Release
mkdir ..\Release\dicom-cs

xcopy /E /EXCLUDE:exclude.txt . ..\Release\dicom-cs