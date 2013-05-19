#!/usr/bin/env python
# -*- coding: utf-8 -*-

from error import handleError

try:
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
					handleError("500 Internal Server Error", 5000, None)
				elif len(ret) == 0:
					handleError("400 Bad Request", 4002, None)
				else:
					print "Status: 303 See Other"
					print "Location: " + ret
					print
					print """
						<!DOCTYPE html>
						<html>
						<head>
						<meta charset="utf-8">
						<title>img.azyobuzi.net</title>
						</head>
						<body>
						<div>
						<img src="%s" alt="Expanded" style="max-width:100%%;max-height:100%%">
						</div>
						</body>
					""" % ret
				
				break
		else:
			handleError("400 Bad Request", 4001, None)
	else:
		handleError("400 Bad Request", 4000, None)
except Exception, ex:
	handleError("500 Internal Server Error", 5001, ex)
