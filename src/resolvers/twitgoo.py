# -*- coding: utf-8 -*-

import re

class twitgoo:
	def __str__(self):
		return "Twitgoo"
	
	regexStr = "^http://(?:www\\.)?twitgoo\\.com/(?:show/\\w+/)?(\\w+)(?:/\\w*/?)?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	def getFullSize(self, match):
		return "http://twitgoo.com/show/img/" + match.group(1)
	
	#miniとthumbは同じ
	
	def getLargeSize(self, match):
		return "http://twitgoo.com/show/thumb/" + match.group(1)
	
	def getThumbnail(self, match):
		return "http://twitgoo.com/show/thumb/" + match.group(1)
