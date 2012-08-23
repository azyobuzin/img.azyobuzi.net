# -*- coding: utf-8 -*-

import re

class yfrog:
	regexStr = "^https?://(?:www\\.)?yfrog\\.com/(\\w+)(?::\\w+)?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	def getFullSize(self, match):
		return "http://yfrog.com/%s:medium" % match.group(1)
	
	def getLargeSize(self, match):
		return "http://yfrog.com/%s:iphone" % match.group(1)
	
	def getThumbnail(self, match):
		return "http://yfrog.com/%s:small" % match.group(1)
