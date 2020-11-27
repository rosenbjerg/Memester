import { FunctionalComponent, h } from "preact";
import { Link } from "preact-router/match";
import * as style from "./style.css";
//import Login from "../login";


const Header: FunctionalComponent = () => {



    return (
        <header class={style.header}>
            <h1>MEMESTER</h1>
            <div class={style.rightControles}>
                {/*<div class={style.loginDiv}>login*/}
                {/*</div>*/}
            </div>
        </header>
    );
};

export default Header;
