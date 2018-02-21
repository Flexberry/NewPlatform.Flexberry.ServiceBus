$documentsPath = [Environment]::GetFolderPath('MyDocuments')
$folderName = 'Flexberry Service Bus'
cd $documentsPath
New-Item -ErrorAction Ignore -Path $folderName -ItemType 'directory'
cd $folderName
$client = new-object System.Net.WebClient
$downloadPath = 'https://raw.githubusercontent.com/Flexberry/NewPlatform.Flexberry.ServiceBus/develop/Docker'
$client.DownloadFile("$downloadPath/flexberry-service-bus-swarm-configuration.yml", "$pwd\flexberry-service-bus-swarm-configuration.yml")
$client.DownloadFile("$downloadPath/start-flexberry-service-bus.cmd", "$pwd\start-flexberry-service-bus.cmd")
$client.DownloadFile("$downloadPath/stop-flexberry-service-bus.cmd", "$pwd\stop-flexberry-service-bus.cmd")
.\start-flexberry-service-bus.cmd
