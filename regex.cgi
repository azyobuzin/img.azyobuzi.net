#!/virtual/azyobuzin/local/bin/python
# -*- coding: utf-8 -*-

from error import handleError

try:
	import json
	import supported
	
	ret = []
	
	for service in supported.services:
		ret.append({"name": str(service), "regex": service.regexStr})
	
	print "Content-Type: application/json"
	print
	print json.dumps(ret, sort_keys = True)
except Exception, ex:
	handleError("500 Internal Server Error", 5001, ex)
