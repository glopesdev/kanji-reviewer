FOR /R %%G IN (kvg/*.svg) DO Transform -s:"kvg/%%~NG.svg" -xsl:kvg2xaml.xsl -o:"xaml/%%~NG.xaml"