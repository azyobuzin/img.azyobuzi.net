# -*- coding: utf-8 -*-

import re

class instagram:
	def __str__(self):
		return "Instagram"
	
	regexStr = "^https?://(?:www\\.)?instagr(?:\\.am|am\\.com)/p/(\\w+)(?:/(?:media/?)?)?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	def getFullSize(self, match):
		return "http://instagr.am/p/%s/media/?size=l" % match.group(1)
	
	def getLargeSize(self, match):
		return "http://instagr.am/p/%s/media/?size=m" % match.group(1)
	
	def getThumbnail(self, match):
		return "http://instagr.am/p/%s/media/?size=t" % match.group(1)
