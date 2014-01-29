param($installPath, $toolsPath, $package, $project)

$dlls = @("bass.dll", "bass_fx.dll", "basscd.dll", "bassenc.dll", "bassflac.dll", "bassmidi.dll", "bassmix.dll", "basswma.dll", "libfftw3-3.dll", "libfftw3f-3.dll", "libfftw3l-3.dll", "tags.dll")

$dirs = @("x64", "x86")

foreach ($dir in $dirs)
{
	foreach ($dll in $dlls)
	{
		$item = $project.ProjectItems.Item($dir).ProjectItems.Item($dll)
		$item.Properties.Item("BuildAction").Value = 2
		$item.Properties.Item("CopyToOutputDirectory").Value = 2
	}
}