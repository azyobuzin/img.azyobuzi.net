#!/usr/bin/env python
# -*- coding: utf-8 -*-

from error import handleError

try:
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
					handleError("500 Internal Server Error", 5000, None)
				else:
					print "Content-Type: application/json"
					print
					print json.dumps({
						"service": unicode(service),
						"full": full,
						"large": large,
						"thumb": thumb
					})
				
				break
		else:
			handleError("400 Bad Request", 4001, None)
	else:
		handleError("400 Bad Request", 4000, None)
except Exception, ex:
	handleError("500 Internal Server Error", 5001, ex)
