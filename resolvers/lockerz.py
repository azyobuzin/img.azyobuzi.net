# -*- coding: utf-8 -*-

import re

class lockerz:
	regexStr = "^https?://(?:www\\.)?(?:lockerz|plixi)\\.com/[sp]/(\\d+)(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	def getFullSize(self, match):
		return "http://api.plixi.com/api/tpapi.svc/imagefromurl?size=big&url=http://lockerz.com/s/" + match.group(1)
	
	def getLargeSize(self, match):
		return "http://api.plixi.com/api/tpapi.svc/imagefromurl?size=medium&url=http://lockerz.com/s/" + match.group(1)
	
	def getThumbnail(self, match):
		return "http://api.plixi.com/api/tpapi.svc/imagefromurl?size=small&url=http://lockerz.com/s/" + match.group(1)
