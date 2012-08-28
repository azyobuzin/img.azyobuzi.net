# -*- coding: utf-8 -*-

import re

class mobypicture:
	def __str__(self):
		return "Mobypicture"
	
	regexStr = "^http://(?:www\\.)?moby\\.to/(\\w+)(?:\\:\\w*)?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	def getFullSize(self, match):
		return "http://moby.to/%s:full" % match.group(1)
	
	def getLargeSize(self, match):
		return "http://moby.to/%s:medium" % match.group(1)
	
	def getThumbnail(self, match):
		return "http://moby.to/%s:thumb" % match.group(1)
