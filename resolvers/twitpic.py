# -*- coding: utf-8 -*-

import re

class twitpic:
	regexStr = "^https?://(?:www\\.)?twitpic\\.com/(?:show/\\w+/)?(\\w+)/?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	def getFullSize(self, match):
		return "http://twitpic.com/show/full/" + match.group(1)
	
	def getLargeSize(self, match):
		return "http://twitpic.com/show/large/" + match.group(1)
	
	def getThumbnail(self, match):
		return "http://twitpic.com/show/thumb/" + match.group(1)
