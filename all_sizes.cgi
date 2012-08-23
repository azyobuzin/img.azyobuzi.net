#!/virtual/azyobuzin/local/bin/python
# -*- coding: utf-8 -*-

import cgitb
cgitb.enable()

import os
import urlparse
import json
import supported

query = dict(urlparse.parse_qsl(os.environ.get("QUERY_STRING", "")))

if "uri" in query:
	uri = query["uri"]
	
	for service in supported.services:
		match = service.regex.match(uri)
		if match is not None:
			full = service.getFullSize(match)
			large = service.getLargeSize(match)
			thumb = service.getThumbnail(match)
			
			if full is None or large is None or thumb is None:
				print "Status: 400 Bad Request"
				print "Content-Type: text/plain"
				print
				print "Could'nt resolve the URI. Maybe the URI is not found."
			else:
				print "Content-Type: application/json"
				print
				print json.dumps({
					"full": full,
					"large": large,
					"thumb": thumb
				})
			
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
