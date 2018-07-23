*** Keywords ***

*** Settings ***
Documentation  <Libraries> 
Library  Selenium2Library
Library  GingerUtils.py

*** Variables ***
${json_string}=  {  "a": "1","b": "2" }

*** Test Cases ***
Sends key and value to Ginger
  [Documentation]  print_ginger_param <key> <value> 
  send ginger param   browser  gc
  
Sends json key,value to Ginger  
  [Documentation]  print ginger params <json <key>,<value> >
  send ginger params json  ${json_string}
  
Set ginger output
  [Documentation]  set ginger output   <key> <value>, print ginger output
  set ginger param  a  1
  set ginger param  b  2
  set ginger param  c  3
  send ginger params 
  reset ginger params
  set ginger param  a  11
  set ginger param  e  22
  set ginger param  f  33
  send ginger params

reset ginger output
  [Documentation]  reset ginger output
  reset ginger params
  
  
 
  
  
  
  
  
  