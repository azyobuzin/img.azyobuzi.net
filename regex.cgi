#!/virtual/azyobuzin/local/bin/python
# -*- coding: utf-8 -*-

import cgitb
cgitb.enable()

import json
import supported

ret = {}

for service in supported.services:
	ret[service.__module__] = service.regexStr

print "Content-Type: application/json"
print
print json.dumps(ret)
