$documentsPath = [Environment]::GetFolderPath('MyDocuments')
$folderName = 'Flexberry Service Bus'
cd $documentsPath
New-Item -ErrorAction Ignore -Path $folderName -ItemType 'directory'
cd $folderName
$client = new-object System.Net.WebClient
$downloadPath = 'https://raw.githubusercontent.com/Flexberry/NewPlatform.Flexberry.ServiceBus/develop/Docker'
$client.DownloadFile("$downloadPath/cloud.yml", "$pwd\cloud.yml")
$client.DownloadFile("$downloadPath/startCloud.cmd", "$pwd\startCloud.cmd")
$client.DownloadFile("$downloadPath/stopCloud.cmd", "$pwd\stopCloud.cmd")
.\startCloud.cmd
