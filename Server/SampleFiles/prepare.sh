# delete the 'First' index
curl -H "x-api-key: default" -X DELETE "http://localhost:9090/First?cleanup=true"

# create the 'First' index
curl -H "x-api-key: default" -d "@index.json" -X POST "http://localhost:9090/indices" 

# retrieve the list of indices
curl -H "x-api-key: default" "http://localhost:9090/indices" 

# add three JSON files to the 'First' index
curl -H "x-api-key: default" -d "@order1.json" -X POST "http://localhost:9090/First?type=json"
curl -H "x-api-key: default" -d "@order2.json" -X POST "http://localhost:9090/First?type=json"
curl -H "x-api-key: default" -d "@order3.json" -X POST "http://localhost:9090/First?type=json"

# get index statistics
curl -H "x-api-key: default" "http://localhost:9090/First/stats" 

# submit query 1
curl -H "x-api-key: default" -d "@query1.json" -X PUT "http://localhost:9090/First"

# retrieve source documents (modify with the appropriate document ID)
curl -H "x-api-key: default" "http://localhost:9090/First/7b61305d-cb30-469a-87f8-83dea2724f73"
curl -H "x-api-key: default" "http://localhost:9090/First/7b61305d-cb30-469a-87f8-83dea2724f73?parsed=true&pretty=true"
