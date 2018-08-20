/* README
 * 
 * Movement mechanics somewhat explained. For more detailed explanation search for Quake / Counter-Strike / Team Fortress 2 tutorials. Keywords; Bhop, Strafejumping, air strafing, surfing.
 * Used in kz and sandbox scenes. Runner scene uses simpler movement with some extras, similar to something like Mirrors edge or Dirty Bomb.
 * 
 * 
 * Player can gain speed various ways but on the ground friction is applied and speed is reduced to normal over time.
 * Player can maintain current speed by jumping. Current code uses queue jumping, meaning that holding space (for each jump) will make player jump automatically next time player lands. Friction calculation are also ignored.
 * Other option for queue jumping would be requiring player to time jumps perfectly, or system that stores inputs for short period of time and therefore requiring timing while still allowing slight mistakes.
 * This might also work better depending on engine used. In source player has to time +jump with exact same tick when landing happens for jump to be considered perfect. But because input jump is actually +jump and -jump commands, even with perfect timing player has 50% change of success.
 * Allowing for margin of error would get rid off such inconsistencies.
 * 
 * 
 * Air strafing;
 * Way to control direction while in the air. Common way of turning is to hold left/right movement keys and move mouse slowly to that direction. Doing so also increases speed of the player.
 * Higher sync between movement keys and mouse results in more speed.
 * Same goes for number of strafes per jump. However, strafing too fast will reduce speed
 * Keep jumping to maintain speed
 * 
 * 
 * Strafe jumping;
 * Used to gain speed fast in a straight line. Hold forward + use left/right keys along with camera movement.
 * Keep jumping to maintain speed
 * 
 * 
 * 
 * Worth noting that with both systems enabled, easiest way is to use strafe jumping as way of gaining speed and airstrafes to controll overall movement.
 * Gaining speed with strafe jumping is limited to more forward motion while bhop allows for gaining speed within a smaller area.
 * 
 * 
 * Surfing;
 * Sliding along angled surfaces. Going downwards will cause player to accelerate and going upwards will deaccelerate player.
 * 
 * 
 * Double jump;
 * Jumping again shortly after jumping results in higher jump. Requires something to jump on or low enough ceiling to shorten the time first jump takes.
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 */