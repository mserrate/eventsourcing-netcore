options({ //option
	resultStreamName: "my_demo_projection_result",
	$includeLinks: false,
	reorderEvents: false,
	processingLag: 0
})

fromStream('account-1') //selector
.when({ //filter
	$init:function(state, evnt){
		return {
			count: 0
		}
	},
	myEventType: function(state, evnt){
		s.count += 1;
	}
})
.transformBy(function(state, evnt){ //transformation
	state.count = 10;
})
.outputState() //transformation
