REM delete the 'First' index
curl -H "x-api-key: default" -X DELETE "http://localhost:9090/First?cleanup=true"

REM create the 'First' index
curl -H "x-api-key: default" -d "@index.json" -X POST "http://localhost:9090/indices" 

REM retrieve the list of indices
curl -H "x-api-key: default" "http://localhost:9090/indices" 

REM add three JSON files to the 'First' index
curl -H "x-api-key: default" -d "@order1.json" -X POST "http://localhost:9090/First/document?type=json"
curl -H "x-api-key: default" -d "@order2.json" -X POST "http://localhost:9090/First/document?type=json"
curl -H "x-api-key: default" -d "@order3.json" -X POST "http://localhost:9090/First/document?type=json"

REM submit query 1
curl -H "x-api-key: default" -d "@query1.json" -X PUT "http://localhost:9090/First"

REM retrieve source documents (modify with the appropriate document ID)
curl -H "x-api-key: default" "http://localhost:9090/First/7b61305d-cb30-469a-87f8-83dea2724f73"
curl -H "x-api-key: default" "http://localhost:9090/First/7b61305d-cb30-469a-87f8-83dea2724f73?parsed=true&pretty=true"
