Follow below steps to install GingerRuntime: 
1.	Download & FTP the GingerRuntime Package to your Linux working directory 
2.	Open Terminal and CD to the folder you placed the package 
3.	Run below commands:
		mkdir GingerRuntime & tar xvf GingerRuntime.tar.gz -C GingerRuntime
		cd GingerRuntime
		sudo ./gingerInstall.sh
4.	Run “ginger” command to check if Ginger Runtime was installed and running

***********************************************************

Steps to install 3rd party required libraries: 
-RHEL run: 
yum install libgdiplus

-Debian run: 
apt-get install libgdiplus
