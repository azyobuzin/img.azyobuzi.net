# -*- coding: utf-8 -*-

import re

class photoshare:
	def __str__(self):
		return "Big Canvas PhotoShare"
	
	regexStr = "^http://(?:www\\.)?bcphotoshare\\.com/photos/\\d+/(\\d+)/?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	#large.jpgとoriginal.jpgは同じ
	#拡張子は関係ない
	
	def getFullSize(self, match):
		return "http://images.bcphotoshare.com/storages/%s/large.jpg" % match.group(1)
	
	def getLargeSize(self, match):
		return "http://images.bcphotoshare.com/storages/%s/large.jpg" % match.group(1)
	
	def getThumbnail(self, match):
		return "http://images.bcphotoshare.com/storages/%s/thumb180.jpg" % match.group(1)
