@ECHO OFF
SETLOCAL
CLS

set /P usr="Enter service account user: "
set /P pwd="Enter service account password: "


ECHO.
ECHO **************************************
ECHO Deleting Crunchbase Service
ECHO **************************************
sc delete "Drone.CrawlDaddy.Service"
ECHO --------------------------------------
ECHO.
ECHO.


ECHO.
ECHO **************************************
ECHO Creating CrawlDaddyService
ECHO **************************************
sc create "Drone.CrawlDaddy.Service" binPath= "\"D:\bizintel-data.int.godaddy.com\Drone\CrawlDaddy\CrawlDaddy.WinService.exe\"" DisplayName= "Drone CrawlDaddy" obj= "%usr%@dc1.corp.gd" password= "%pwd%"
ECHO Setting CrawlDaddy Service description
ECHO --------------------------------------
sc description "Drone.CrawlDaddy.Service" "CrawlDaddy Service"
ECHO.
ECHO.


pause