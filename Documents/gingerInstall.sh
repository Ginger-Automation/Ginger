echo "Installing Ginger Runtime"

if [ "$EUID" -ne 0 ]
  then echo "Please run as root"
  exit
fi

FILE=/bin/ginger

GingerInstallDirectory=/opt/ginger

if test -f "$FILE"; then
 rm /bin/ginger
fi


if [[ -d "$GingerInstallDirectory" ]]
then
	echo ""
    echo "$GingerInstallDirectory exists on your filesystem. Deleting it's content"
	rm -r $GingerInstallDirectory
fi


mkdir $GingerInstallDirectory
tar -xzf GingerRuntime.tar.gz -C $GingerInstallDirectory

chmod 777 /opt/ginger/GingerRuntime

ln -s /opt/ginger/GingerRuntime /bin/ginger 
echo ""
echo ""
echo "Congratulations!! Ginger Runtime is successfully installed."
echo ""
echo "You can start by typing ginger in your terminal"
echo ""
echo "For more info go to https://ginger.amdocs.com in your browser"
echo ""
echo ""
echo "Trying to install thrid party dependencies. If it fails please read readme.txt"
echo ""
echo ""
if command -v apt-get >/dev/null; then
	
  apt-get install libgdiplus
elif command -v yum >/dev/null; then
	yum install libgdiplus
else
  echo "Unknown system, please install 3rd Party dependencies manually"
fi

