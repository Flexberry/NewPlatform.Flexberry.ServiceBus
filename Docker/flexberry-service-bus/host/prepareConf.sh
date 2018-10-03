set -x
export confFile=/opt/flexberry-service-bus/NewPlatform.Flexberry.ServiceBus.WinServiceHost.exe.config
export newConfFile=/tmp/conf.$$
export tmpFile=/tmp/tmp_$$
cp $confFile $newConfFile 

function replaceConf() {
  sed -e "s|$1|$2|" <$newConfFile  >$tmpFile 2>/tmp/sederrors
  if [ -s '/tmp/sederrors' ]
  then
    cat /tmp/sederrors
  fi
  mv $tmpFile $newConfFile
} 

replaceConf 'http://flexberry-service-bus:' "http://$DOCKER_HOSTNAME:"

if xmllint $newConfFile >/dev/null 2>/tmp/xmlerror
then
  mv $newConfFile $confFile
else
  cat /tmp/xmlerror
fi 
