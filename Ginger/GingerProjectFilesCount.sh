#!/bin/bash

usage()
{
    echo "usage: script [[-c create ] | [-h help]]"
}

if [ $# -gt 0 ]; then
    echo "Your command line contains $# arguments"

    case $1 in
        -c | --create )         shift
								echo "working on creating input files... "
								rm filesList.cnt 
								find . -name *.cs | grep ".cs"  | grep -v Debug | grep -v Release > filesList.cnt
								cat filesList.cnt | cut -d '/' -f 2 | sort -u > projectNames.lst
								echo "input file creation - done "
                                ;;
        -h | --help )           usage
                                exit
                                ;;
        * )                     usage
                                exit 1
    esac    
else
    echo "Your command line contains no arguments"
fi



##### Main

echo "Processing each project to find count of c# files : "
echo "--"

for n in $(cat projectNames.lst)
do
    #echo "Working on $n file name now"
    x=`cat filesList.cnt | grep "\/$n\/" | grep -v Debug | grep -v Release | wc -l`
    #y=`cat filesList.cnt | cut -d '/' -f 2 | grep $n | grep -v Debug | grep -v Release | wc -l`

    echo "$n : $x"
done