import { h } from "preact";
import * as style from "./style.css";

const fullscreenStyle = { width: "100%", height: "100%" };
const normalStyle = { display: "inline-block" };

const  Loading = ({ fullscreen, title }: { fullscreen?: boolean, title:string }) => (
    <span style={fullscreen ? fullscreenStyle : normalStyle}>
        <span class={style.loadingContainer}>
            <div>{title}</div>
            <div class={style["sk-folding-cube"]}>
                <div class={style["sk-cube"]}/>
                <div class={`${style["sk-cube2"]} ${style["sk-cube"]}`}/>
                <div class={`${style["sk-cube4"]} ${style["sk-cube"]}`}/>
                <div class={`${style["sk-cube3"]} ${style["sk-cube"]}`}/>
            </div>
        </span>
    </span>

)

export default Loading;