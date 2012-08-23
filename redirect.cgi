#!/virtual/azyobuzin/local/bin/python
# -*- coding: utf-8 -*-

import cgitb
cgitb.enable()

import os
import urlparse
import supported

query = dict(urlparse.parse_qsl(os.environ.get("QUERY_STRING", "")))

if "uri" in query:
	uri = query["uri"]
	size = query["size"] if "size" in query else "full"
	
	for service in supported.services:
		match = service.regex.match(uri)
		if match is not None:
			ret = ""
			
			if size == "full":
				ret = service.getFullSize(match)
			elif size == "large":
				ret = service.getLargeSize(match)
			elif size == "thumb":
				ret = service.getThumbnail(match)
			
			if ret is None:
				print "Status: 400 Bad Request"
				print "Content-Type: text/plain"
				print
				print "Could'nt resolve the URI. Maybe the URI is not found."
			elif len(ret) == 0:
				print "Status: 400 Bad Request"
				print "Content-Type: text/plain"
				print
				print "\"size\" parameter is invalid."
			else:
				print "Location: " + ret
				print
			
			break
	else:
		print "Status: 400 Bad Request"
		print "Content-Type: text/plain"
		print
		print "Could'nt resolve the URI. Maybe not supported."
else:
	print "Status: 400 Bad Request"
	print "Content-Type: text/plain"
	print
	print "\"uri\" parameter is required."