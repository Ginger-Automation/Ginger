import json
import csv
from robot.api import logger
from collections import OrderedDict
from json import JSONDecoder

gingerParamMap = OrderedDict()


#*******************************
#   ** PRINT PARAMS **
#*******************************

def Write_To_Console(s):
    logger.console(s)
    
def Print_Value(key,value):
  logger.console(key + " = " + value)  
    

#*******************************
#   ** GET GINGER PARAMS **
#*******************************

def Get_Ginger_Param(filename,KeyInput):
    if filename is None:
        return "invalid file name"
    if KeyInput is None:
        return "invalid key"
    json_object = json.load(open(filename))
#    logger.console(json_object) 
    for key in json_object:
          if KeyInput == key:
            value = json_object[key]
            return  value
    return "param not found"  

    
#*******************************
#   ** SET GINGER PARAMS **
#*******************************

def Set_Ginger_Param(key,value):
    if key is not None:    
      gingerParamMap[key]=value
   

#*******************************
#   ** RESET GINGER PARAMS **
#*******************************

def Reset_Ginger_Params():
    gingerParamMap.clear()
    

#*******************************
#   ** SEND GINGER PARAMS **
#*******************************

def Send_Ginger_Param(key,value):
  logger.console("~~~GINGER_RC_START~~~")
  logger.console(key + "=" + value)
  logger.console("~~~GINGER_RC_END~~~") 


def Send_Ginger_Params():
  logger.console("~~~GINGER_RC_START~~~")
  for key in gingerParamMap:
    value = gingerParamMap[key]
    logger.console(key + "=" + value)
  logger.console("~~~GINGER_RC_END~~~")    


def Send_Ginger_Params_JSON(json_str):
  logger.console("~~~GINGER_RC_START~~~")
  json_object = json.loads(json_str)
  for key in json_object:
    value = json_object[key]
    logger.console(key + "=" + value)
  logger.console("~~~GINGER_RC_END~~~")
  



  
#def get_ginger_param(filename,KeyInput):
#    with open(filename, 'rb') as f:
#      reader = csv.reader(f)
#      for row in reader: 
#        if row is not None and row[0] is not None:  
#          if row[0] == KeyInput:
#            return row[1]
#    return "param not found"  




   


    
    