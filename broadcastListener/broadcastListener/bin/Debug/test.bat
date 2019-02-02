start broadcastListener.exe "{  
   \"broadcastPort_Tool\":20001,
   \"broadcastPort_Slaves\":20002,
   \"enableGUI\":true,
   \"enablePrintSlave\":false,
   \"enablePrintTool\":false,
   \"enablePrintStatus\":true,
   \"replicatorsList\":[  
      {  
         \"ip_string\":\"192.168.1.100\",
         \"id\":8796095578122,
         \"isSelf\":true
      },
      {  
         \"ip_string\":\"192.168.1.101\",
         \"id\":8796095578123,
         \"isSelf\":false
      },
      {  
         \"ip_string\":\"192.168.1.102\",
         \"id\":8796095578124,
         \"isSelf\":false
      }
   ],
   \"myPartitionNumber\":0,
   \"partitionNumbers_Total\":2,
   \"toolReceiverThreads\":1,
   \"slaveReceiverThreads\":1,
   \"broadcastAddress\":\"192.168.1.255\",
   \"logFile\":   \"C:\\Users\\diama\\Desktop\\Listener0_EventLog.txt\",
   \"errFile\":   \"C:\\Users\\%USERNAME%\\Desktop\\Listener0_Errors.txt\",
   \"criticalErrFile\":   \"C:\\Users\\%USERNAME%\\Desktop\\Listener0_CriticalErrors.txt\",
   \"toolMsgFile\":null,
   \"slaveMsgFile\":null,
   \"slaveNotifyMode_Batch\":100,
   \"dinamicallyStarted\":false,
   \"logToolMsgOnReceive\":false,
   \"exclusiveBind\":false,
   \"KafkaNodes\":   \"http://localhost:9093, http://localhost:9094, http://localhost:9095\",
   \"KafkaTopic\":   \"toolsEvents\",
   \"benchmark\":true
}"