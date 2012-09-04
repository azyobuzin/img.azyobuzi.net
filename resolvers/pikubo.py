# -*- coding: utf-8 -*-

import re

class pikubo:
	def __str__(self):
		return "Pikubo"
	
	regexStr = "^https?://(?:www\\.)?pikubo\\.jp/photo/(\\w+)(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	def getFullSize(self, match):
		return "http://pikubo.jp/p/l/" + match.group(1)
	
	def getLargeSize(self, match):
		return "http://pikubo.jp/p/l/" + match.group(1)
	
	def getThumbnail(self, match):
		return "http://pikubo.jp/p/q/" + match.group(1)
