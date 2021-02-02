const app = require('express')()
const fs=require('fs')
var udp=require('dgram');

porthttp=process.env.PORT || 3000;
portudp=process.env.PORT || 2222;

function render(filename, params={}) {
    var data = fs.readFileSync(filename, 'binary');
    for (var key in params) {
      data = data.replace('{' + key + '}', params[key]);
    }
    return data;
}
app.get('/', (req, res) => {
    res.send(render('index.htm'));
})
app.get('/download', (req, res) => {
    res.send(render('game/bin/Debug/game.exe'));
})
/** key down */

app.get('/left', (req, res) => {
    var name=req.query.name;
    players[name].left=true;
    players[name].right=false;
    res.send("");
})
app.get('/right', (req, res) => {
    var name=req.query.name;
    players[name].right=true;
    players[name].left=false;
    res.send("");
})
app.get('/top', (req, res) => {
    var name=req.query.name;
    players[name].top=true;
    players[name].bottom=false;
    res.send("");
})
app.get('/bottom', (req, res) => {
    var name=req.query.name;
    players[name].bottom=true;
    players[name].top=false;
    res.send("");
})

/** key up */

app.get('/leftUp', (req, res) => {
    var name=req.query.name;
    players[name].left=false;
    res.send("");
})
app.get('/rightUp', (req, res) => {
    var name=req.query.name;
    players[name].right=false;
    res.send("");
})
app.get('/topUp', (req, res) => {
    var name=req.query.name;
    players[name].top=false;
    res.send("");
})
app.get('/bottomUp', (req, res) => {
    var name=req.query.name;
    players[name].bottom=false;
    res.send("");
})

/** click */

app.get('/click', (req, res) => {
    var name=req.query.name;
    var click=players[name].click
    var x=parseInt(req.query.x);
    var y=parseInt(req.query.y);
    click.check=true;
    click.x=x;
    click.y=y;
    res.send("");
})

app.get('/port', (req, res) => {
    res.send(portudp);
})

app.listen(porthttp,() => {
    console.log('http://127.0.0.1:3000/')
    update();
})

var server=udp.createSocket('udp4');

range=600;
psize=40;
bsize=16;
pspeed=3;
bspeed=7;
players={};

server.on('message',(msg,info)=>{
    if(msg in players){
        server.send("fail",info.port,info.address);
        return; 
    }
    players[msg]={
        "info":info,
        "x":Math.floor(Math.random()*(range-psize)),
        "y":Math.floor(Math.random()*(range-psize)),
        "left":false,
        "right":false,
        "top":false,
        "bottom":false,
        "click":{
            "check":false,
            "x":0,
            "y":0
        },
        "bullets":[]
    }
})
server.bind(portudp);


function update(){
    var cmds="";
    for(var name in players){
        player=players[name];
        if(player.left==true){
            player.x-=pspeed;
            if(player.x<0)player.x=0;
        }
        if(player.right==true){
            player.x+=pspeed;
            if(player.x>range-psize)player.x=range-psize;
        }
        if(player.top==true){
            player.y-=pspeed;
            if(player.y<0)player.y=0;
        }
        if(player.bottom==true){
            player.y+=pspeed;
            if(player.y>range-psize)player.y=range-psize;
        }
        if(player.click.check){
            let p0=[player.x+psize/2-bsize/2,player.y+psize/2-bsize/2]
            let p1=[player.click.x-bsize/2,player.click.y-bsize/2]
            let x=(p1[0]-p0[0])
            let y=(p1[1]-p0[1])
            let dis=Math.sqrt(x*x+y*y)
            player.bullets.push({
                "x":p0[0],
                "y":p0[1],
                "dir":{
                    "x":x/dis*bspeed,
                    "y":y/dis*bspeed
                }
            })
            player.click.check=false;
        }
        
        var bullets=player.bullets;
        for(var i=0;i<bullets.length;i++){
            bullets[i].x+=bullets[i].dir.x
            bullets[i].y+=bullets[i].dir.y
            for(var n2 in players){
                let p2=players[n2];
                if(n2==name)continue;
                if(bullets[i].x>p2.x-bsize&&bullets[i].x<p2.x+psize)
                    if(bullets[i].y>p2.y-bsize&&bullets[i].y<p2.y+psize){
                        cmds+="remove "+n2+' ';
                        delete players[n2];
                    }
            }
            if(bullets[i].x<-bsize||bullets[i].y<-bsize||bullets[i].x>range||bullets[i].y>range){
                bullets.splice(i,1);
                i--;
            }
        }
    }
    for(var name in players){
        player=players[name]
        cmds+=name+' '+player.x+' '+player.y+' '+player.bullets.length+' ';
        let bullets=player.bullets;
        for(var i=0;i<bullets.length;i++){
            cmds+=Math.ceil(bullets[i].x)+' '+Math.ceil(bullets[i].y)+' ';
        }
    }
    for(var name in players){
        let info=players[name].info;
        server.send(cmds,info.port,info.address);
    }
    setTimeout(update,15);
}
