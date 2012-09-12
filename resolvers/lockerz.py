# -*- coding: utf-8 -*-

import re

class lockerz:
	def __str__(self):
		return "Lockerz"
	
	regexStr = "^https?://(?:(?:www\\.|pics\\.)?lockerz\\.com/(?:s|u/\\d+/photos)|(?:www\\.)?plixi\\.com/p)/(\\d+)(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	def getFullSize(self, match):
		return "http://api.plixi.com/api/tpapi.svc/imagefromurl?size=big&url=http://lockerz.com/s/" + match.group(1)
	
	def getLargeSize(self, match):
		return "http://api.plixi.com/api/tpapi.svc/imagefromurl?size=medium&url=http://lockerz.com/s/" + match.group(1)
	
	def getThumbnail(self, match):
		return "http://api.plixi.com/api/tpapi.svc/imagefromurl?size=small&url=http://lockerz.com/s/" + match.group(1)
