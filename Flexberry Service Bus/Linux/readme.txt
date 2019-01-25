Для установки сервиса в linux:
Cкопировать файлы сервиса на сервер (не забыть удалить Mono.Security.dll)
Положить файл flexberryservicebus в папку /etc/init.d/
Исправить значения переменных:
MONOSERVER - путь до mono-service2, если будет использоваться mono из пакетов то прописать $(which mono-service2)
SERVICEPATH - путь до файлов сервиса шины
USER - пользователь из под которого должен работать сервис

Выполнить:
chkconfig --add flexberryservicebus
chkconfig --level 2345 flexberryservicebus on

После этого chkconfig --list flexberryservicebus должен выводить
flexberryservicebus       0:выкл  1:выкл  2:вкл   3:вкл   4:вкл   5:вкл   6:выкл

Можно выполнять
sudo service flexberryservicebus start

Так же, за счет chkconfig, служба будет автоматически запускаться при старте системы

TODO
на будущее можно сделать в сервисе обработку сигналов для pause и resume http://manpages.ubuntu.com/manpages/trusty/man1/mono-service2.1.html#contenttoc3 и 
http://stackoverflow.com/questions/3660039/success-with-start-stop-daemon-and-mono-service2
