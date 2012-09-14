# -*- coding: utf-8 -*-

import re
import urllib2

class lockerz:
	def __str__(self):
		return "Lockerz"
	
	regexStr = "^https?://(?:(?:www\\.|pics\\.)?lockerz\\.com/(?:s|u/\\d+/photos)|(?:www\\.)?plixi\\.com/p)/(\\d+)(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	def getFullSize(self, match):
		return "http://api.plixi.com/api/tpapi.svc/imagefromurl?size=big&url=" + urllib2.quote("http://lockerz.com/s/" + match.group(1))
	
	def getLargeSize(self, match):
		return "http://api.plixi.com/api/tpapi.svc/imagefromurl?size=medium&url=" + urllib2.quote("http://lockerz.com/s/" + match.group(1))
	
	def getThumbnail(self, match):
		return "http://api.plixi.com/api/tpapi.svc/imagefromurl?size=small&url=" + urllib2.quote("http://lockerz.com/s/" + match.group(1))
